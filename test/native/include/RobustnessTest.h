#pragma once

// Verifies that invalid input to the C API (malformed fonts and images, NULL handles, invalid sizes and text arguments)
// is rejected with a NULL or default result instead of crashing.
bool robustness_test();
