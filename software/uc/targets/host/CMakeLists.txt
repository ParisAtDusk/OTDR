
add_executable(host
  src/main.c
  src/hardware_io.c
  src/board_gpio.c
)

target_link_libraries(host PRIVATE app_core)

target_include_directories(host PRIVATE ${CMAKE_CURRENT_SOURCE_DIR}/inc)

target_link_options(host PRIVATE -Wl,--gc-sections)
target_compile_definitions(led_anim PUBLIC LED_ANIM_TASK_STACK=4096)
