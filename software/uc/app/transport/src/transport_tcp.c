#include "transport_tcp.h"
#include "result.h"

#include <errno.h>
#include <netinet/in.h>
#include <string.h>
#include <sys/socket.h>
#include <sys/types.h>
#include <unistd.h>

// clang-format off

typedef struct {
  int listen_socket; /* bound/listening socket, valid for the transport's lifetime */
  int socket;         /* accepted client connection, -1 until a client attaches */
  uint16_t port;
} tcp_context_t;

/* Device side: bind + listen once, then wait for a client. Safe to call
 * repeatedly -- if the listening socket already exists, this just blocks on
 * accept() again for a new client, which is how reconnects are handled after
 * one drops (see tcp_receive's R_ErrorClosed handling). */
static Result tcp_open(transport_t *transport) {
  tcp_context_t *ctx = transport->context;

  if (ctx->listen_socket < 0) {
    ctx->listen_socket = socket(AF_INET, SOCK_STREAM, 0);
    if (ctx->listen_socket < 0) {
      return R_ErrorSocketCreate;
    }

    int opt = 1;
    setsockopt(ctx->listen_socket, SOL_SOCKET, SO_REUSEADDR, &opt, sizeof(opt));

    struct sockaddr_in addr;
    memset(&addr, 0, sizeof(addr));
    addr.sin_family = AF_INET;
    addr.sin_addr.s_addr = INADDR_ANY;
    addr.sin_port = htons(ctx->port);

    if (bind(ctx->listen_socket, (struct sockaddr *)&addr, sizeof(addr)) < 0) {
      close(ctx->listen_socket);
      ctx->listen_socket = -1;
      return R_ErrorBind;
    }

    if (listen(ctx->listen_socket, 1) < 0) {
      close(ctx->listen_socket);
      ctx->listen_socket = -1;
      return R_ErrorListen;
    }
  }

  /* Blocks here until a client connects -- the first time, or again after a
   * previous client disconnected. */
  ctx->socket = accept(ctx->listen_socket, NULL, NULL);
  if (ctx->socket < 0) {
    return R_ErrorAccept;
  }

  return R_Success;
}

static Result tcp_close(transport_t *transport) {
  tcp_context_t *ctx = transport->context;

  if (ctx->socket < 0 && ctx->listen_socket < 0) {
    return R_ErrorNotOpen;
  }

  if (ctx->socket >= 0) {
    close(ctx->socket);
    ctx->socket = -1;
  }
  if (ctx->listen_socket >= 0) {
    close(ctx->listen_socket);
    ctx->listen_socket = -1;
  }

  return R_Success;
}

static Result tcp_send(transport_t *transport, const uint8_t *data,
                       size_t length) {
  tcp_context_t *ctx = transport->context;

  if (ctx->socket < 0) {
    return R_ErrorNotOpen;
  }

  size_t total_sent = 0;
  while (total_sent < length) {
    ssize_t sent = send(ctx->socket, data + total_sent, length - total_sent, 0);
    if (sent < 0) {
      if (errno == EINTR) {
        continue; /* interrupted by signal, retry */
      }
      return R_ErrorSend;
    }
    if (sent == 0) {
      /* Peer closed the connection. */
      return R_ErrorClosed;
    }
    total_sent += (size_t)sent;
  }

  return R_Success;
}

static Result tcp_receive(
  transport_t *transport,
  uint8_t *data,
  size_t capacity, size_t *received)
{
  tcp_context_t *ctx = transport->context;

  *received = 0;

  if (ctx->socket < 0) {
    tcp_open(transport);
    return R_ErrorNotOpen;
  }

  ssize_t n = recv(ctx->socket, data, capacity, 0);
  if (n < 0) {
    if (errno == EINTR) {
      return R_Success; /* nothing read this time; caller can retry */
    }
    return R_ErrorReceive;
  }
  if (n == 0) {
    /* Peer performed an orderly shutdown -- close it so it's not left
     * around returning EOF forever; app_transport_task will call
     * transport_open() again to accept a new client. */
    close(ctx->socket);
    ctx->socket = -1;
    return R_ErrorClosed;
  }

  *received = (size_t)n;
  return R_Success;
}

static const transport_ops_t tcp_ops = {
    .open = tcp_open,
    .close = tcp_close,
    .send = tcp_send,
    .receive = tcp_receive,
};

static tcp_context_t context;

Result transport_tcp_init(transport_t *transport, uint16_t port) {
  if (transport == NULL) {
    return R_ErrorInvalidArgument;
  }

  context.port = port;
  context.socket = -1;
  context.listen_socket = -1;

  transport->ops = &tcp_ops;
  transport->context = &context;

  return R_Success;
}
