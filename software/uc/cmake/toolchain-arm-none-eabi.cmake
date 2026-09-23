set(CMAKE_SYSTEM_NAME Generic)
set(CMAKE_SYSTEM_PROCESSOR)

set(CMAKE_C_COMPILER   "${ARM_TOOLCHAIN_PATH}/bin/arm-none-eabi-gcc")
set(CMAKE_CXX_COMPILER "${ARM_TOOLCHAIN_PATH}/bin/arm-none-eabi-g++")
set(CMAKE_ASM_COMPILER "${ARM_TOOLCHAIN_PATH}/bin/arm-none-eabi-gcc")

set(CMAKE_AR      "${ARM_TOOLCHAIN_PATH}/bin/arm-none-eabi-ar")
set(CMAKE_OBJCOPY "${ARM_TOOLCHAIN_PATH}/bin/arm-none-eabi-objcopy")
set(CMAKE_OBJDUMP "${ARM_TOOLCHAIN_PATH}/bin/arm-none-eabi-objdump")
set(CMAKE_SIZE    "${ARM_TOOLCHAIN_PATH}/bin/arm-none-eabi-size")

set(CMAKE_TRY_COMPILE_TARGET_TYPE STATIC_LIBRARY)

set(MCU_FLAGS
    -mcpu=cortex-m7
    -mthumb
    -mfpu=fpv5-d16
    -mfloat-abi=hard
)

add_compile_options(
    ${MCU_FLAGS}
    -ffunction-sections
    -fdata-sections
)

add_link_options(
    ${MCU_FLAGS}
    -Wl,--gc-sections
)
set(CMAKE_C_STANDARD 11)
set(CMAKE_C_STANDARD_REQUIRED ON)
set(CMAKE_FIND_ROOT_PATH "${ARM_TOOLCHAIN_PATH}")
set(CMAKE_FIND_ROOT_PATH_MODE_PROGRAM NEVER)
set(CMAKE_FIND_ROOT_PATH_MODE_LIBRARY ONLY)
set(CMAKE_FIND_ROOT_PATH_MODE_INCLUDE ONLY)
set(CMAKE_FIND_ROOT_PATH_MODE_PACKAGE ONLY)
