#include "scpi_commands_otdr.h"
#include "result.h"
#include "scpi/parser.h"
#include "scpi/scpi.h" // IWYU pragma: keep
#include "scpi/types.h"
#include "scpi/units.h"
#include "transport_if.h"
#include <stddef.h>
#include <stdint.h>
#include <stdlib.h>

#define SCPI_IDN1 "AGH"
#define SCPI_IDN2 "OTDR"
#define SCPI_IDN3 "001"      /* Serial Number */
#define SCPI_IDN4 "00.00.01" /* Software Version TODO: move it to CMake*/

#define SCPI_ERROR_QUEUE_SIZE 17
static scpi_error_t scpi_error_queue_data[SCPI_ERROR_QUEUE_SIZE];

#define SCPI_INPUT_BUFFER_LENGTH 256
static char scpi_input_buffer[SCPI_INPUT_BUFFER_LENGTH];

typedef struct {
  transport_t *transport;
} scpi_user_context_t;

static scpi_user_context_t scpi_user_context;
static scpi_t scpi_context;

static size_t s_scpi_write(scpi_t *scpi, const char *data, size_t length) {
  scpi_user_context_t *ctx = scpi->user_context;

  Result result = transport_send(ctx->transport, (const uint8_t *)data, length);

  return result == R_Success ? length : 0;
}

// clang-format off
static const scpi_command_t scpi_commands[] = {
  /* IEEE 488.2 mandatory commands */
  { .pattern = "*CLS",  .callback = SCPI_CoreCls,  },
  { .pattern = "*ESE",  .callback = SCPI_CoreEse,  },
  { .pattern = "*ESE?", .callback = SCPI_CoreEseQ, },
  { .pattern = "*ESR?", .callback = SCPI_CoreEsrQ, },
  { .pattern = "*IDN?", .callback = SCPI_CoreIdnQ, },
  { .pattern = "*OPC",  .callback = SCPI_CoreOpc,  },
  { .pattern = "*OPC?", .callback = SCPI_CoreOpcQ, },
  { .pattern = "*RST",  .callback = SCPI_CoreRst,  },
  { .pattern = "*SRE",  .callback = SCPI_CoreSre,  },
  { .pattern = "*SRE?", .callback = SCPI_CoreSreQ, },
  { .pattern = "*STB?", .callback = SCPI_CoreStbQ, },
  { .pattern = "*TST?", .callback = SCPI_CoreTstQ, },
  { .pattern = "*WAI",  .callback = SCPI_CoreWai,  },

  /* SCPI required commands */
  { .pattern = "SYSTem:ERRor[:NEXT]?",
    .callback = SCPI_SystemErrorNextQ, },

  { .pattern = "SYSTem:ERRor:COUNt?",
    .callback = SCPI_SystemErrorCountQ, },

  { .pattern = "SYSTem:VERSion?",
    .callback = SCPI_SystemVersionQ, },

  SCPI_CMD_LIST_END
};

scpi_interface_t scpi_interface = {
  .write = s_scpi_write,
  .error = NULL,
  .reset = NULL,
};
// clang-format on

Result SCPI_CoreInit(transport_t *transport) {
  scpi_user_context.transport = transport;
  SCPI_Init(&scpi_context, scpi_commands, &scpi_interface, scpi_units_def,
            SCPI_IDN1, SCPI_IDN2, SCPI_IDN3, SCPI_IDN4, scpi_input_buffer,
            SCPI_INPUT_BUFFER_LENGTH, scpi_error_queue_data,
            SCPI_ERROR_QUEUE_SIZE);
  // SCPI_Init corrupts user_context
  scpi_context.user_context = &scpi_user_context;
  scpi_context.interface->reset = NULL; // TODO: Add reset callback
  scpi_context.interface->error = NULL; // TODO: Add error callback with logging
  return R_Success;
}

Result SCPI_CoreConsume(const uint8_t *data, size_t data_len) {
  SCPI_Input(&scpi_context, (const char *)data, data_len);
  return R_Success;
}
