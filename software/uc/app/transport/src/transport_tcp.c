#include "transport_tcp.h"
#include "result.h"

// clang-format off

typedef struct {
  int socket;
} tcp_context_t;

static Result tcp_open(transport_t *transport) {
  tcp_context_t *ctx = transport->context;

  /* create/connect socket */
  // ctx->socket = /* ... */;
  return R_ErrorNotImplemented;
}

static Result tcp_close(transport_t *transport) {
  tcp_context_t *ctx = transport->context;

  /* close(ctx->socket); */
  return R_ErrorNotImplemented;
}

static Result tcp_send(transport_t *transport, const uint8_t *data,
                       size_t length) {
  tcp_context_t *ctx = transport->context;

  /* return send(ctx->socket, data, length, 0); */
  return R_ErrorNotImplemented;
}

static Result tcp_receive(
  transport_t *transport,
  uint8_t *data,
  size_t capacity, size_t *received)
{
  tcp_context_t *ctx = transport->context;

  /* ... */

  *received = 0;
  return R_ErrorNotImplemented;
}

static const transport_ops_t tcp_ops = {
    .open = tcp_open,
    .close = tcp_close,
    .send = tcp_send,
    .receive = tcp_receive,
};

static tcp_context_t context;

Result transport_tcp_init(transport_t *transport) {

  transport->ops = &tcp_ops;
  transport->context = &context;

  return R_Success;
}
