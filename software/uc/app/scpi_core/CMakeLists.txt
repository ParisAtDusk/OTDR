add_library(scpi_core STATIC
    # scpi_dispatch.c
    # scpi_commands_otdr.c
    # trace_buffer.c # shared with cm4 memory?
)
target_include_directories(scpi_core PUBLIC ${CMAKE_CURRENT_SOURCE_DIR})
target_link_libraries(scpi_core PUBLIC scpi_parser transport_if acquisition_if)
