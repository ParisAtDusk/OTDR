# app/app

add_library(app_core STATIC
    src/app.c
)

target_include_directories(app_core PUBLIC inc)

target_link_libraries(app_core
    PUBLIC
        scpi_core
        transport_if
        freertos_kernel
        freertos_config
)
