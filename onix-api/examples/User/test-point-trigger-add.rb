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
  WalletId: "0b2e0a83-9e47-4bd4-ac4b-c9b7b977aaa7",
  EventTriggered: "CustomerRegistered",
  PointRuleInput: {
    ProductQuantity: 1,
  },
}

result = make_request(:post, apiUrl, param)

json_string = result.to_json

puts(json_string)
