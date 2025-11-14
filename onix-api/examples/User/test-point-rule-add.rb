#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']

apiUrl = "api/PointRule/org/#{orgId}/action/AddPointRule"
param =  {
  RuleName: "CalculateRegisteredPoint1",
  Description: "Calculate point for scan item register",
  Tags: "Register",
  TriggeredEvent: "CustomerRegistered",
  RuleDefinition: "RuleDefinition",
}

result = make_request(:post, apiUrl, param)
puts(result)
