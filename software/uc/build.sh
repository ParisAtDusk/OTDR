#!/bin/bash

if ! cmake -S . -B build; then
  exit 1
fi

if ! cmake --build build; then
  echo "Build failed!"
  exit 1
fi
