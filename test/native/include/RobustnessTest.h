#pragma once

#include <filesystem>

// Verifies that invalid input to the C API (malformed fonts and images, NULL handles, invalid sizes and text arguments)
// is rejected with a NULL or default result instead of crashing, and that fonts can be used from several threads.
bool robustness_test(const std::filesystem::path &test_data_path);
