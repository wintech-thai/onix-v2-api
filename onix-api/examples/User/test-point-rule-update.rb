#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']
id = '99207826-3b58-4f52-aecb-4254d6dc5b56'

 
ruleDef = <<EOF
- WorkflowName: "Customer Register Point Rate"
  Rules:
    - RuleName: "15 point per unit rate"
      Expression: "input.ProductQuantity > 0"
      Actions:
        - Name: "AssignOutput"
          Context:
            OutputExpression: "input.ProductQuantity * 15"
EOF

apiUrl = "api/PointRule/org/#{orgId}/action/UpdatePointRuleById/#{id}"
param =  {
  RuleName: "CalculateRegisteredPoint3",
  Description: "Calculate point for scan item register",
  Tags: "Register,ALDMX",
  TriggeredEvent: "CustomerRegistered",
  RuleDefinition: ruleDef,
}

result = make_request(:post, apiUrl, param)
puts(result)
