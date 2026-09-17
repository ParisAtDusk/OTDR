#ifndef _SCPI_COMMANDS_OTDR_H
#define _SCPI_COMMANDS_OTDR_H

#include "scpi/scpi.h"

typedef enum {
  SCPI_IDN,
  SCPI_RST,
  SCPI_ACQ_START,
  SCPI_ACQ_STOP,
  SCPI_ACQ_AVG
} ScpiCommand_e;

int test(int a, int b);

#endif
