// app.c
#include "app.h"
#include "projdefs.h"
#include "result.h"

#include "FreeRTOS.h"
#include "hardware.h"
#include "task.h"
#include "transport_if.h"

#include <stdio.h>
#include <unistd.h>

static transport_t *s_transport;
static TaskHandle_t s_console_task_handle;
static TaskHandle_t s_transport_task_handle;
static DeviceState_e s_state = Disconnected;

static void app_console_task(void *arg) {
  (void)arg;
  uint32_t counter = 0;

  for (;;) {
    printf("app: tick=%lu counter=%lu\n\r", (unsigned long)xTaskGetTickCount(),
           (unsigned long)counter++);
    vTaskDelay(pdMS_TO_TICKS(1000));
  }
}

// clang-format off

static void update_outputs_task(void *arg){
  (void)arg;
  for(;;){
    switch (s_state) {
      case Disconnected: break;
      default: break;
    }
    vTaskDelay(pdMS_TO_TICKS(100));
  }
}

// clang-format on

static void app_transport_task(void *arg) {
  (void)arg;
  uint8_t buf[256];

  for (;;) {
    size_t received = 0;
    Result r = transport_receive(s_transport, buf, sizeof(buf), &received);

    if (ResSuccess(r) && received > 0) {
      printf("app: rx %lu bytes\n\r", (unsigned long)received);
      transport_send(s_transport, buf, received);
    } else if (r == R_Pending || (ResSuccess(r) && received == 0)) {
      vTaskDelay(pdMS_TO_TICKS(10));
    } else if (r == R_ErrorClosed) {
      printf("app: client disconnected, waiting for reconnect...\n\r");
      Result open_r;
      do {
        open_r = transport_open(s_transport);
        if (open_r == R_Pending) {
          vTaskDelay(pdMS_TO_TICKS(50));
        }
      } while (open_r == R_Pending);

      if (ResSuccess(open_r)) {
        printf("app: client reconnected\n\r");
      } else {
        printf("app: reconnect failed: %d\n\r", (int)open_r);
        vTaskDelay(pdMS_TO_TICKS(500));
      }
    } else if (ResIsError(r)) {
      printf("app: transport_receive returned %d\n\r", (int)r);
      vTaskDelay(pdMS_TO_TICKS(500));
    }
  }
}

Result app_init(transport_t *transport) {
  if (transport == NULL) {
    return R_ErrorGeneric;
  }

  s_transport = transport;

  Result open_r;
  do {
    open_r = transport_open(s_transport);
    if (open_r == R_Pending) {
      usleep(50 * 1000);
    }
  } while (open_r == R_Pending);

  if (!ResSuccess(open_r)) {
    return R_ErrorInit;
  }

  BaseType_t console_ok =
      xTaskCreate(app_console_task, "console", 512, NULL, tskIDLE_PRIORITY + 1,
                  &s_console_task_handle);

  BaseType_t transport_ok =
      xTaskCreate(app_transport_task, "transport", 512, NULL,
                  tskIDLE_PRIORITY + 2, &s_transport_task_handle);

  return (console_ok == pdPASS && transport_ok == pdPASS) ? R_Success
                                                          : R_ErrorGeneric;
}

void app_run(void) {
  vTaskStartScheduler();
  for (;;) {
  } // should never get here
}
