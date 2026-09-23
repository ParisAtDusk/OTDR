

if(TARGET_PLATFORM STREQUAL "host")
  add_library(transport_if STATIC src/transport_tcp.c)
elseif(TARGET_PLATFORM STREQUAL "cm7")
  add_library(transport_if STATIC src/transport_usb.c)
endif()
target_include_directories(transport_if
    PUBLIC
        ${CMAKE_CURRENT_SOURCE_DIR}/inc
)

target_link_libraries(transport_if
    PUBLIC
        common
)
