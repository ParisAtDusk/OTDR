#ifndef _APP_H_
#define _APP_H_

#include "result.h"
#include "transport_if.h"

Result app_init(transport_t *transport);
void app_run(void);

#endif // !_APP_H_
