#ifndef _TRANSPORT_IF_H_
#define _TRANSPORT_IF_H_

#include "result.h"
#include <stddef.h>
#include <stdint.h>

// clang-format off

typedef struct transport transport_t;

typedef struct {
  Result (*open)(transport_t *transport);
  Result (*close)(transport_t *transport);
  Result (*send)(transport_t *transport, const uint8_t *data, size_t length);
  Result (*receive)(transport_t *transport, uint8_t *data, size_t capacity, size_t *received);
} transport_ops_t;

struct transport {
  const transport_ops_t *ops;
  void *context;
};

static inline Result transport_open(transport_t *t)
{
  return t->ops->open(t);
}

static inline Result transport_close(transport_t *t)
{
  return t->ops->close(t);
}

static inline Result transport_send(
  transport_t *t,
  const uint8_t *data,
  size_t length)
{
  return t->ops->send(t, data, length);
}

static inline Result transport_receive(
  transport_t *t,
  uint8_t *data,
  size_t capacity, size_t *received)
{
  return t->ops->receive(t, data, capacity, received);
}

#endif
