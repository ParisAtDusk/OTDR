#include "app.h"
#include "result.h"

#include "FreeRTOS.h"
#include "task.h"

#include <stdio.h>

static TaskHandle_t s_console_task_handle;

static void app_console_task(void *arg) {
  (void)arg;
  uint32_t counter = 0;

  for (;;) {
    printf("app: tick=%lu counter=%lu\n\r", (unsigned long)xTaskGetTickCount(),
           (unsigned long)counter++);
    vTaskDelay(pdMS_TO_TICKS(1000));
  }
}

Result app_init(void) {
  BaseType_t ok = xTaskCreate(app_console_task, "console", 512, NULL,
                              tskIDLE_PRIORITY + 1, &s_console_task_handle);
  return (ok == pdPASS) ? R_Success : R_ErrorGeneric;
}

void app_run(void) {
  vTaskStartScheduler();
  for (;;) {
  } // should never get here
}
