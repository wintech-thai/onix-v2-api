#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']
ruleDef = <<EOF
[
  {
    "WorkflowName": "workflow1",
    "Rules": [
      {
        "RuleName": "GiveDiscount10",
        "Expression": "input.ProductQuantity > 0",
        "SuccessEvent": 10,
        "Actions": {
          "OnSuccess": {
            "Name": "OutputExpression",
            "Context": {
              "Expression": "100"
            }
          }
        }
      }
    ]
  }
] 
EOF

apiUrl = "api/PointRule/org/#{orgId}/action/AddPointRule"
param =  {
  RuleName: "CalculateRegisteredPoint2",
  Description: "Calculate point for scan item register",
  Tags: "Register",
  TriggeredEvent: "CustomerRegistered",
  RuleDefinition: ruleDef,
}

result = make_request(:post, apiUrl, param)
puts(result)
