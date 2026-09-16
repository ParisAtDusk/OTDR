# TOP LEVEL CMAKE

cmake_minimum_required(VERSION 3.22)

set(TARGET_PLATFORM "host" CACHE STRING
    "cm7 | cm4 | host ")

if(TARGET_PLATFORM STREQUAL "cm7" OR TARGET_PLATFORM STREQUAL "cm4")
    set(MCU_FAMILY "H7")
else()
    set(MCU_FAMILY "")   # host build has no MCU
endif()

if(NOT TARGET_PLATFORM STREQUAL "host")
    set(CMAKE_TOOLCHAIN_FILE ${CMAKE_SOURCE_DIR}/cmake/toolchain-arm-none-eabi.cmake)
endif()

project(otdr C CXX ASM)

add_subdirectory(third_party)
add_subdirectory(app)
add_subdirectory(targets)

if(TARGET_PLATFORM STREQUAL "host")
    add_subdirectory(tests/unit_tests)
endif()
