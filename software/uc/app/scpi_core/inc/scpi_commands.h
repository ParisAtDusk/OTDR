#ifndef SCPI_COMMANDS_H
#define SCPI_COMMANDS_H

#include "transport_if.h"
#include <stdint.h>

// TODO: Implement OTDR specific commands
// typedef enum {
//   SCPI_IDN,
//   SCPI_RST,
//   SCPI_ACQ_START,
//   SCPI_ACQ_STOP,
//   SCPI_ACQ_AVG
// } ScpiCommand_e;

Result SCPI_CoreInit(transport_t *transport);

Result SCPI_CoreConsume(const uint8_t *data, size_t data_len);

#endif
