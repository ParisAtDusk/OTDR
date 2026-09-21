add_library(led_anim)

target_sources(led_anim
  PRIVATE
    src/led_anim.c
    src/led_anim_core.c
    src/led_out_gpio.c

  INTERFACE

)
