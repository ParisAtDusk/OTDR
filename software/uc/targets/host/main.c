#include "scpi_commands_otdr.h"
#include "transport_if.h"
#include "transport_tcp.h"
#include <stdint.h>
#include <stdio.h>

transport_t transport;

int main(void) {
  int ui = test(1, 2);
  transport_tcp_init(&transport);
  transport_open(&transport);
  char data[] = "test data\n\r";
  transport_send(&transport, (uint8_t *)&data, sizeof(data));
  printf("Hello = %d\n\r", ui);
  return 0;
}
