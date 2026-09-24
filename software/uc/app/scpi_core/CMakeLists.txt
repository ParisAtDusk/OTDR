# app/scpi_core

add_library(scpi_core STATIC
    # scpi_dispatch.c
    src/scpi_commands.c
    src/scpi_commands_otdr.c
    # trace_buffer.c # shared with cm4 memory?
)
target_include_directories(scpi_core PUBLIC ${CMAKE_CURRENT_SOURCE_DIR}/inc)

target_link_libraries(scpi_core PUBLIC
  common
  scpi_parser
  transport_if
  ) # acquisition_if)
