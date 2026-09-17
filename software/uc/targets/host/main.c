#include "FreeRTOS.h"
#include "scpi_commands_otdr.h"
#include "task.h"
#include "transport_if.h"
#include "transport_tcp.h"
#include <stdint.h>
#include <stdio.h>

static void test_task(void *arg) {
  while (1) {
    printf("Hello from FreeRTOS\n");
    fflush(stdout);

    vTaskDelay(pdMS_TO_TICKS(1000));
  }
}

transport_t transport;

int main(void) {
  int ui = test(1, 2);
  transport_tcp_init(&transport);
  transport_open(&transport);
  char data[] = "test data\n\r";
  transport_send(&transport, (uint8_t *)&data, sizeof(data));
  printf("Hello = %d\n\r", ui);

  xTaskCreate(test_task, "Test", 1024, NULL, 1, NULL);

  vTaskStartScheduler();
  return 0;
}
