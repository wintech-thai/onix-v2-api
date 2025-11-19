#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']

apiUrl = "api/PointTrigger/org/#{orgId}/action/AddPointTrigger"
param =  {
  WalletId: "0e41781b-ca87-4780-98b6-94f4d27b5fef",
  EventTriggered: "CustomerRegistered",
  PointRuleInput: {
    ProductQuantity: 1,
  },
}

result = make_request(:post, apiUrl, param)
puts(result)
