add_library(common INTERFACE)

target_include_directories(common INTERFACE ${CMAKE_CURRENT_SOURCE_DIR}/common)

add_subdirectory(transport)
add_subdirectory(scpi_core)
