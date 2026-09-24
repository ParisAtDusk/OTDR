#ifndef SCPI_COMMANDS_OTDR_H
#define SCPI_COMMANDS_OTDR_H
#include "result.h"
#include "scpi/types.h"
#include "trace.h"
#include <stdint.h>

typedef struct {
  Result (*acq_start)(void);
  Result (*acq_stop)(void);
  Result (*acq_iters)(uint32_t);
  Result (*acq_iters_query)(uint32_t *);
  Result (*acq_pulsewidth)(uint32_t);
  Result (*acq_pulsewidth_query)(uint32_t *);
  Result (*acq_laserpower)(uint32_t);
  Result (*acq_laserpower_query)(uint32_t *);
  Result (*trace_data_query)(trace_t *);
} scpi_otdr_api_t;

scpi_result_t AcqStart(scpi_t *context);
scpi_result_t AcqStop(scpi_t *context);
scpi_result_t AcqIters(scpi_t *context);
scpi_result_t AcqItersQ(scpi_t *context);
scpi_result_t AcqPulseWidth(scpi_t *context);
scpi_result_t AcqPulseWidthQ(scpi_t *context);
scpi_result_t AcqLaserPower(scpi_t *context);
scpi_result_t AcqLaserPowerQ(scpi_t *context);
scpi_result_t TraceDataQ(scpi_t *context);

// TODO: add power units to conform to standard

#define SCPI_COMMANDS(X)                                                       \
  X("ACQuire:STARt", AcqStart)                                                 \
  X("ACQuire:STOP", AcqStop)                                                   \
  X("ACQuire:PARameter:ITERations", AcqIters)                                  \
  X("ACQuire:PARameter:ITERations?", AcqItersQ)                                \
  X("ACQuire:PARameter:PULSe:WIDTh", AcqPulseWidth)                            \
  X("ACQuire:PARameter:PULSe:WIDTh?", AcqPulseWidthQ)                          \
  X("ACQuire:PARameter:PULSe:POWer", AcqLaserPower)                            \
  X("ACQuire:PARameter:PULSe:POWer?", AcqLaserPowerQ)                          \
  X("TRACe:DATA?", TraceDataQ)

#define SCPI_ENTRY(p, cb) {.pattern = p, .callback = cb},

void RegisterOtdrApi(const scpi_otdr_api_t *api);

#endif // !SCPI_COMMANDS_OTDR_H
