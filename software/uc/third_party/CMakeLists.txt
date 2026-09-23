# third_party/CMakeLists.txt

add_library(cmsis_core INTERFACE)
target_include_directories(cmsis_core INTERFACE CMSIS/CMSIS/Core/Include)

# scpi_parser and FreeRTOS are both excluded on cm4: cm4 is bare-metal and
# doesn't run SCPI or the RTOS at all
if(NOT TARGET_PLATFORM STREQUAL "cm4")
  add_library(scpi_parser STATIC
      scpi-parser/libscpi/src/parser.c
      scpi-parser/libscpi/src/lexer.c
      scpi-parser/libscpi/src/error.c
      scpi-parser/libscpi/src/ieee488.c
      scpi-parser/libscpi/src/minimal.c
      scpi-parser/libscpi/src/units.c
      scpi-parser/libscpi/src/utils.c
      scpi-parser/libscpi/src/fifo.c
      scpi-parser/libscpi/src/expression.c
  )
  target_include_directories(scpi_parser PUBLIC scpi-parser/libscpi/inc)

  if(TARGET_PLATFORM STREQUAL "host")
    set(FREERTOS_PORT "GCC_POSIX" CACHE STRING "" FORCE)
    set(FREERTOS_HEAP "3" CACHE STRING "" FORCE)
  endif()

  add_subdirectory(FreeRTOS-Kernel)
  target_link_libraries(freertos_kernel PUBLIC freertos_config)
  target_link_libraries(freertos_kernel_port PUBLIC freertos_config)
endif()

if(MCU_FAMILY STREQUAL "H7")
  add_library(cmsis_device_h7 STATIC
        cmsis_device_h7/Source/Templates/system_stm32h7xx.c)

  target_compile_definitions(cmsis_device_h7 PUBLIC STM32H757xx)
  target_include_directories(cmsis_device_h7 PUBLIC cmsis_device_h7/Include)
  target_link_libraries(cmsis_device_h7 PUBLIC cmsis_core)

  add_library(mcu_ll STATIC
      stm32h7xx_ll_driver/Src/stm32h7xx_ll_gpio.c
      stm32h7xx_ll_driver/Src/stm32h7xx_ll_rcc.c
      stm32h7xx_ll_driver/Src/stm32h7xx_ll_utils.c
      stm32h7xx_ll_driver/Src/stm32h7xx_ll_usart.c
      stm32h7xx_ll_driver/Src/stm32h7xx_ll_dma.c
      stm32h7xx_ll_driver/Src/stm32h7xx_ll_exti.c
  )
  # add_library(mcu_ll STATIC ${LL_SOURCES})
  target_compile_definitions(mcu_ll PUBLIC STM32H757xx)
  target_include_directories(mcu_ll PUBLIC stm32h7xx_ll_driver/Inc)
  target_link_libraries(mcu_ll PUBLIC cmsis_device_h7)
endif()
