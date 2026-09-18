#include "app.h"
#include "result.h"
#include "transport_if.h"
#include "transport_tcp.h"

int main(void) {
  transport_t tcp;
  transport_tcp_init(&tcp, 2137);
  if (ResIsError(app_init(&tcp)))
    return 1;
  app_run();
  return 0;
}
