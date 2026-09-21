add_library(led_anim STATIC
  src/led_anim_core.c
  src/led_anim.c
  src/led_out_gpio.c
)

target_include_directories(led_anim PUBLIC inc)
target_link_libraries(led_anim PUBLIC freertos_kernel hardware_io)
