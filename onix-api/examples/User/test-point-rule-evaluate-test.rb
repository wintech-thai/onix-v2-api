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
              "Expression": "300"
            }
          }
        }
      }
    ]
  }
] 
EOF

apiUrl = "api/PointRule/org/#{orgId}/action/TestPointRule"
param = {
  ProductQuantity: 1,
  RuleDefinition: ruleDef,
}

result = make_request(:post, apiUrl, param)
puts(result)
