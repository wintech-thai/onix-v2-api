#!/bin/bash

rm -rf TestResults
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:./TestResults/**/coverage.cobertura.xml -targetdir:coveragereport -reporttypes:Html
