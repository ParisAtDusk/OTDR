add_library(common INTERFACE)

target_include_directories(common INTERFACE
  ${CMAKE_SOURCE_DIR}/common/helpers/include)

add_library(hardware_io INTERFACE)

target_include_directories(hardware_io INTERFACE
  ${CMAKE_SOURCE_DIR}/common/hardware/include)

add_subdirectory(transport)
add_subdirectory(scpi_core)
add_subdirectory(core)
add_subdirectory(led_anim)
