
add_executable(host src/main.c)

target_link_libraries(host PRIVATE app_core)

target_include_directories(host PRIVATE ${CMAKE_CURRENT_SOURCE_DIR}/inc)

target_link_options(host PRIVATE -Wl,--gc-sections)

