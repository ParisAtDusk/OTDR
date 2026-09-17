# third_party/CMakeLists.txt

add_library(cmsis_core INTERFACE)
target_include_directories(cmsis_core INTERFACE CMSIS/CMSIS/Core/Include)

add_library(scpi_parser STATIC
    scpi-parser/libscpi/src/parser.c
    scpi-parser/libscpi/src/lexer.c
    scpi-parser/libscpi/src/error.c
    scpi-parser/libscpi/src/ieee488.c
    scpi-parser/libscpi/src/minimal.c
    scpi-parser/libscpi/src/units.c
    scpi-parser/libscpi/src/utils.c
    scpi-parser/libscpi/src/expression.c
)
target_include_directories(scpi_parser PUBLIC scpi-parser/libscpi/inc)

# if(NOT TARGET_PLATFORM STREQUAL "host")
add_subdirectory(FreeRTOS-Kernel)
# endif()

if(MCU_FAMILY STREQUAL "H7")
  add_library(cmsis_device_h7 STATIC
        cmsis_device_h7/Source/Templates/system_stm32h7xx.c)

  target_include_directories(cmsis_device_h7 PUBLIC cmsis_device_h7/Include)
  target_link_libraries(cmsis_device_h7 PUBLIC cmsis_core)

  file(GLOB LL_SOURCES stm32h7xx_ll_driver/Src/stm32h7xx_ll_*.c)
  add_library(mcu_ll STATIC ${LL_SOURCES})
  target_include_directories(mcu_ll PUBLIC stm32h7xx_ll_driver/Inc)
  target_link_libraries(mcu_ll PUBLIC cmsis_device_h7)
endif()
