#!/usr/bin/env bash
# Copyright (c) Cratis. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.
set -euo pipefail
cd "$(dirname "$0")/.."
exec python3 Benchmarks/run.py "$@"
