/*
 * _tools/bench/libyaml_bench.c - the libyaml (C) runner for the throughput
 * benchmark. Same arguments and JSON row as the .NET runner:
 *
 *   libyaml_bench throughput <root> <corpus> <seconds> <ser|deser|both>
 *
 * deser loads every file into a yaml_document_t (the libyaml node tree, the
 * C counterpart of the untyped .NET trees) and frees it. ser walks trees
 * loaded before the clock starts and emits them through the event API into
 * a reused memory buffer, keeping each node's scalar and collection style.
 * input_bytes counts UTF-16 code units, as the .NET runner does, so MB/s is
 * comparable across runners.
 * Pure ASCII.
 */
#define _POSIX_C_SOURCE 200809L

#include <dirent.h>
#include <errno.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>

#include <yaml.h>

typedef struct {
    unsigned char *data;
    size_t size;
    size_t units;
} text_t;

typedef struct {
    unsigned char *data;
    size_t size;
    size_t capacity;
} buffer_t;

static void die(const char *what, const char *detail) {
    fprintf(stderr, "libyaml_bench: %s%s%s\n", what, detail ? ": " : "", detail ? detail : "");
    exit(2);
}

static double now(void) {
    struct timespec ts;
    clock_gettime(CLOCK_MONOTONIC, &ts);
    return (double)ts.tv_sec + (double)ts.tv_nsec / 1e9;
}

static size_t utf16_units(const unsigned char *data, size_t size) {
    size_t units = 0;
    for (size_t i = 0; i < size; i++) {
        unsigned char c = data[i];
        if ((c & 0xC0) != 0x80) {
            units += (c >= 0xF0) ? 2 : 1;
        }
    }
    return units;
}

static int yaml_file(const struct dirent *entry) {
    size_t length = strlen(entry->d_name);
    return length > 5 && strcmp(entry->d_name + length - 5, ".yaml") == 0;
}

static int by_name(const struct dirent **left, const struct dirent **right) {
    return strcmp((*left)->d_name, (*right)->d_name);
}

static text_t *read_corpus(const char *directory, size_t *count) {
    struct dirent **entries;
    int found = scandir(directory, &entries, yaml_file, by_name);
    if (found < 0) {
        die("cannot list", directory);
    }
    if (found == 0) {
        die("no files in", directory);
    }

    text_t *texts = calloc((size_t)found, sizeof(text_t));
    for (int i = 0; i < found; i++) {
        char path[4096];
        snprintf(path, sizeof(path), "%s/%s", directory, entries[i]->d_name);
        FILE *file = fopen(path, "rb");
        if (!file) {
            die("cannot open", path);
        }
        fseek(file, 0, SEEK_END);
        long size = ftell(file);
        fseek(file, 0, SEEK_SET);
        texts[i].data = malloc((size_t)size + 1);
        if (fread(texts[i].data, 1, (size_t)size, file) != (size_t)size) {
            die("cannot read", path);
        }
        fclose(file);
        texts[i].size = (size_t)size;
        texts[i].units = utf16_units(texts[i].data, texts[i].size);
        free(entries[i]);
    }
    free(entries);
    *count = (size_t)found;
    return texts;
}

static void load(const text_t *text, yaml_document_t *document) {
    yaml_parser_t parser;
    if (!yaml_parser_initialize(&parser)) {
        die("parser init failed", NULL);
    }
    yaml_parser_set_input_string(&parser, text->data, text->size);
    if (!yaml_parser_load(&parser, document)) {
        die("parse failed", parser.problem);
    }
    yaml_parser_delete(&parser);
}

static int write_buffer(void *data, unsigned char *chunk, size_t size) {
    buffer_t *buffer = data;
    if (buffer->size + size > buffer->capacity) {
        size_t capacity = buffer->capacity ? buffer->capacity : 4096;
        while (buffer->size + size > capacity) {
            capacity *= 2;
        }
        buffer->data = realloc(buffer->data, capacity);
        buffer->capacity = capacity;
    }
    memcpy(buffer->data + buffer->size, chunk, size);
    buffer->size += size;
    return 1;
}

static void emit(yaml_emitter_t *emitter, yaml_event_t *event) {
    if (!yaml_emitter_emit(emitter, event)) {
        die("emit failed", emitter->problem);
    }
}

static void emit_node(yaml_emitter_t *emitter, yaml_document_t *document, yaml_node_t *node) {
    yaml_event_t event;
    switch (node->type) {
        case YAML_SCALAR_NODE:
            yaml_scalar_event_initialize(&event, NULL, NULL,
                node->data.scalar.value, (int)node->data.scalar.length,
                1, 1, node->data.scalar.style);
            emit(emitter, &event);
            break;
        case YAML_SEQUENCE_NODE:
            yaml_sequence_start_event_initialize(&event, NULL, NULL, 1, node->data.sequence.style);
            emit(emitter, &event);
            for (yaml_node_item_t *item = node->data.sequence.items.start;
                 item < node->data.sequence.items.top; item++) {
                emit_node(emitter, document, yaml_document_get_node(document, *item));
            }
            yaml_sequence_end_event_initialize(&event);
            emit(emitter, &event);
            break;
        case YAML_MAPPING_NODE:
            yaml_mapping_start_event_initialize(&event, NULL, NULL, 1, node->data.mapping.style);
            emit(emitter, &event);
            for (yaml_node_pair_t *pair = node->data.mapping.pairs.start;
                 pair < node->data.mapping.pairs.top; pair++) {
                emit_node(emitter, document, yaml_document_get_node(document, pair->key));
                emit_node(emitter, document, yaml_document_get_node(document, pair->value));
            }
            yaml_mapping_end_event_initialize(&event);
            emit(emitter, &event);
            break;
        default:
            die("unexpected node type", NULL);
    }
}

static size_t dump(yaml_document_t *document, buffer_t *buffer) {
    yaml_emitter_t emitter;
    yaml_event_t event;
    buffer->size = 0;
    if (!yaml_emitter_initialize(&emitter)) {
        die("emitter init failed", NULL);
    }
    yaml_emitter_set_output(&emitter, write_buffer, buffer);
    yaml_emitter_set_unicode(&emitter, 1);
    yaml_emitter_set_width(&emitter, -1);
    yaml_emitter_set_indent(&emitter, 2);

    yaml_stream_start_event_initialize(&event, YAML_UTF8_ENCODING);
    emit(&emitter, &event);
    yaml_document_start_event_initialize(&event, NULL, NULL, NULL, 1);
    emit(&emitter, &event);
    yaml_node_t *root = yaml_document_get_root_node(document);
    if (root) {
        emit_node(&emitter, document, root);
    } else {
        yaml_scalar_event_initialize(&event, NULL, NULL, (yaml_char_t *)"~", 1, 1, 1, YAML_PLAIN_SCALAR_STYLE);
        emit(&emitter, &event);
    }
    yaml_document_end_event_initialize(&event, 1);
    emit(&emitter, &event);
    yaml_stream_end_event_initialize(&event);
    emit(&emitter, &event);
    yaml_emitter_flush(&emitter);
    yaml_emitter_delete(&emitter);
    return buffer->size;
}

static int throughput(const char *root, const char *corpus, double seconds, const char *mode) {
    int ser = strcmp(mode, "ser") == 0;
    int deser = strcmp(mode, "deser") == 0;
    int both = strcmp(mode, "both") == 0;
    if (!ser && !deser && !both) {
        fprintf(stderr, "unknown throughput mode '%s' (expected ser, deser, or both)\n", mode);
        return 2;
    }

    char directory[4096];
    snprintf(directory, sizeof(directory), "%s/%s", root, corpus);
    size_t count;
    text_t *texts = read_corpus(directory, &count);
    size_t bytes = 0;
    for (size_t i = 0; i < count; i++) {
        bytes += texts[i].units;
    }

    buffer_t buffer = {0};
    yaml_document_t *trees = calloc(count, sizeof(yaml_document_t));
    for (size_t i = 0; i < count; i++) {
        load(&texts[i], &trees[i]);
        dump(&trees[i], &buffer);
        if (!ser) {
            yaml_document_delete(&trees[i]);
        }
    }

    long documents = 0;
    long passes = 0;
    long long produced = 0;
    double start = now();
    double elapsed = 0;
    while ((elapsed = now() - start) < seconds) {
        for (size_t i = 0; i < count; i++) {
            if (ser) {
                produced += (long long)dump(&trees[i], &buffer);
            } else {
                yaml_document_t document;
                load(&texts[i], &document);
                if (both) {
                    produced += (long long)dump(&document, &buffer);
                }
                yaml_document_delete(&document);
            }
            documents++;
        }
        passes++;
    }
    elapsed = now() - start;

    printf("{\"parser\":\"libyaml\",\"corpus\":\"%s\",\"mode\":\"%s\",\"seconds\":%.3f,"
           "\"documents\":%ld,\"input_bytes\":%zu,\"output_bytes\":%lld,\"passes\":%ld,"
           "\"documents_per_second\":%.1f,\"mb_per_second\":%.2f}\n",
        corpus, mode, elapsed, documents, bytes, produced, passes,
        (double)documents / elapsed, (double)bytes * (double)passes / 1048576.0 / elapsed);

    if (ser) {
        for (size_t i = 0; i < count; i++) {
            yaml_document_delete(&trees[i]);
        }
    }
    for (size_t i = 0; i < count; i++) {
        free(texts[i].data);
    }
    free(trees);
    free(texts);
    free(buffer.data);
    return 0;
}

int main(int argc, char **argv) {
    if (argc < 2) {
        printf("libyaml benchmark runner (libyaml %s)\n\n"
               "Usage:\n"
               "  libyaml_bench throughput <root> <corpus> [seconds] [ser|deser|both]\n\n"
               "Defaults: seconds=10, mode=both. Emits one JSON row, like the .NET runner.\n",
            yaml_get_version_string());
        return 0;
    }
    if (strcmp(argv[1], "throughput") != 0) {
        fprintf(stderr, "unknown command '%s'\n", argv[1]);
        return 2;
    }
    if (argc < 4) {
        fprintf(stderr, "usage: libyaml_bench throughput <root> <corpus> (run with no args for help)\n");
        return 2;
    }
    double seconds = argc > 4 ? strtod(argv[4], NULL) : 10;
    const char *mode = argc > 5 ? argv[5] : "both";
    return throughput(argv[2], argv[3], seconds, mode);
}
