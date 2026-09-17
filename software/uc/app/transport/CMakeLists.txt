add_library(transport_if STATIC
    src/transport_tcp.c
)

target_include_directories(transport_if
    PUBLIC
        ${CMAKE_CURRENT_SOURCE_DIR}/inc
)

target_link_libraries(transport_if
    PUBLIC
        common
)
