#!/bin/bash

rm -rf TestResults
dotnet test --collect:"XPlat Code Coverage"

reportgenerator -reports:./TestResults/**/coverage.cobertura.xml \
-classfilters:"-onix.api.Migrations.*" \
-targetdir:coveragereport -reporttypes:Html
