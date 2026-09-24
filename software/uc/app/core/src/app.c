// app.c
#include "app.h"
#include "board_gpio.h"
#include "led_anim.h"
#include "led_out_gpio.h"
#include "otdr.h"
#include "portmacro.h"
#include "projdefs.h"
#include "result.h"

#include "FreeRTOS.h" // IWYU pragma: keep
#include "led_anim.h"
#include "scpi_commands.h"
#include "scpi_commands_otdr.h"
#include "task.h"
#include "transport_if.h"

#include <stdbool.h>
#include <stdio.h>
#include <unistd.h>

static transport_t *s_transport;
static TaskHandle_t s_console_task_handle;
static TaskHandle_t s_transport_task_handle;
// static DeviceState_e s_state = Disconnected;

static LedHandle_t AddGpio(GpioPin_t *pin) {
  LedOut_t out = LedOutGpio_Make(pin);
  return LedAnim_Add(&out);
}

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

static void LedTask(void *arg)
{
    (void)arg;

    GpioInit(&led_status);
    GpioInit(&led_fault);
    GpioInit(&led_rx);

    LedHandle_t green = AddGpio(&led_status);
    LedHandle_t red   = AddGpio(&led_fault);
    LedHandle_t blue  = AddGpio(&led_rx);

    LedAnim_Blink(green, 100, 900, 0);        // 100 ms on / 900 ms off, forever
    LedAnim_Fade(blue, 1000, 1500, 0, 255);   // rise 1 s, fall 1.5 s, forever, full brightness
    LedAnim_On(red);

    vTaskDelay(pdMS_TO_TICKS(5000));

    LedAnim_Blink(red, 200, 200, 5);          // 5 pulses, then off
    LedAnim_Off(green);                       // cancels the blink
    LedAnim_Fade(blue, 500, 500, 3, 128);     // 3 cycles at half brightness, then off

    vTaskDelay(portMAX_DELAY);
}

static const scpi_otdr_api_t _api = {
  .acq_iters = acquire_set_iterations,
  .acq_iters_query = acquire_get_iterations,
  .acq_start = acquire_start,
  .acq_stop = acquire_stop,
  .acq_pulsewidth = acquire_set_pulse_width,
  .acq_pulsewidth_query = acquire_get_pulse_width,
  .acq_laserpower = acquire_set_laser_power,
  .acq_laserpower_query = acquire_get_laser_power,
  .trace_data_query = trace_get_data,
};

// clang-format on

static void app_transport_task(void *arg) {
  (void)arg;
  uint8_t buf[256];

  for (;;) {
    size_t received = 0;
    Result r = transport_receive(s_transport, buf, sizeof(buf), &received);

    if (ResSuccess(r) && received > 0) {
      printf("app: rx %lu bytes\n\r", (unsigned long)received);
      // taskENTER_CRITICAL();
      SCPI_CoreConsume(buf, received);
      // taskEXIT_CRITICAL();
      // transport_send(s_transport, buf, received);
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
      usleep(50 * 1000); // TODO: won't work on stm
    }
  } while (open_r == R_Pending);

  if (!ResSuccess(open_r)) {
    return R_ErrorInit;
  }

  bool err = LedAnim_Init(tskIDLE_PRIORITY + 1);
  if (err == false)
    return R_ErrorInit;

  SCPI_CoreInit(s_transport);
  RegisterOtdrApi(&_api);

  BaseType_t console_ok =
      xTaskCreate(app_console_task, "console", 512, NULL, tskIDLE_PRIORITY + 1,
                  &s_console_task_handle);

  BaseType_t transport_ok =
      xTaskCreate(app_transport_task, "transport", 512, NULL,
                  tskIDLE_PRIORITY + 2, &s_transport_task_handle);

  BaseType_t leds_ok = xTaskCreate(LedTask, "leds", configMINIMAL_STACK_SIZE,
                                   NULL, tskIDLE_PRIORITY + 1, NULL);

  return (console_ok == pdPASS && transport_ok == pdPASS && leds_ok == pdPASS)
             ? R_Success
             : R_ErrorInit;
}

void app_run(void) {
  vTaskStartScheduler();
  for (;;) {
  } // should never get here
}
