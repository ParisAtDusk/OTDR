
add_executable(host
    main.c
    # tasks/task_scpi.c
    # tasks/task_usb.c
    # tasks/task_ipc.c
    # port_usb_ll/transport_usb.c
    # port_ipc_cm4/acquisition_ipc.c
    # something more ??
)

target_link_libraries(host PRIVATE
    scpi_core
)

target_link_options(host PRIVATE
    -Wl,--gc-sections
)

