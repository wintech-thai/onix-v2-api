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
              "Expression": "200"
            }
          }
        }
      },
      {
        "RuleName": "PRODUCT-A002-250-POINTS",
        "Expression": "(input.ProductCode == \\"PROD-002\\")",
        "Actions": {
          "OnSuccess": {
            "Name": "OutputExpression2",
            "Context": {
              "Expression": "input.ProductQuantity * 250"
            }
          }
        }
      }
    ]
  }
] 
EOF

apiUrl = "api/PriceRule/org/#{orgId}/action/AddPriceRule"
param =  {
  RuleName: "DefaultPriceRate#3",
  Description: "Defalut flat rate for each product",
  Tags: "PriceRule, Default",
  TriggeredEvent: "CalculatePrice",
  RuleDefinition: ruleDef,
}

result = make_request(:post, apiUrl, param)
puts(result)
