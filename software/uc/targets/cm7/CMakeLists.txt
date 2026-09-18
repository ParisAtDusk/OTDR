add_executable(cm7
    src/main.c
)

target_link_libraries(cm7 PRIVATE
    stm32h7xx_ll
    cmsis_device_h7
)

target_link_options(cm7 PRIVATE
    -T${CMAKE_CURRENT_SOURCE_DIR}/STM32H755XIHx_CM7.ld
    -Wl,--gc-sections
    -Wl,-Map=${CMAKE_CURRENT_BINARY_DIR}/cm7.map
)

target_include_directories(cm7
    PRIVATE
        ${CMAKE_CURRENT_SOURCE_DIR}/inc
)

add_custom_command(TARGET cm7 POST_BUILD
    COMMAND ${CMAKE_OBJCOPY} -O binary $<TARGET_FILE:cm7> cm7.bin
)
