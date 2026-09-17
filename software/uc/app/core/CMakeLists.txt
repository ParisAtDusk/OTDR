# app/app

add_library(otdr_app_core STATIC
  src/app.c)

target_include_directories(otdr_app_core PUBLIC ${CMAKE_CURRENT_SOURCE_DIR}/inc)

target_link_libraries(otdr_app_core PUBLIC common)
