add_executable(otdr_cm7
    main.c
    # tasks/task_scpi.c
    # tasks/task_usb.c
    # tasks/task_ipc.c
    # port_usb_ll/transport_usb.c
    # port_ipc_cm4/acquisition_ipc.c
    # something more ??
)

target_link_libraries(otdr_cm7 PRIVATE
    scpi_core
    freertos_kernel
    stm32h7xx_ll
    cmsis_device_h7
)

target_link_options(otdr_cm7 PRIVATE
    -T${CMAKE_CURRENT_SOURCE_DIR}/STM32H755XIHx_CM7.ld
    -Wl,--gc-sections
    -Wl,-Map=${CMAKE_CURRENT_BINARY_DIR}/otdr_cm7.map
)

add_custom_command(TARGET otdr_cm7 POST_BUILD
    COMMAND ${CMAKE_OBJCOPY} -O binary $<TARGET_FILE:otdr_cm7> otdr_cm7.bin
)
