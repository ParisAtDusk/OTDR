/* transport_tcp.h */

#ifndef TRANSPORT_TCP_H
#define TRANSPORT_TCP_H

#include "transport_if.h"
// clang-format off
Result transport_tcp_init(
  transport_t *transport,
  uint16_t port);

#endif
