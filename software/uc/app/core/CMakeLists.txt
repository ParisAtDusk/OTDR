# app/core

add_library(app_core STATIC
    src/app.c
)

target_include_directories(app_core PUBLIC inc)

target_link_libraries(app_core
    PUBLIC
      common
      transport_if
      # hardware_io
      led_anim
    PRIVATE
        scpi_core
        freertos_kernel
        freertos_config
)
