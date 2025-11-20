#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']
token = 'xxxxxx'

apiUrl = "api/PointTrigger/org/#{orgId}/action/AddPointTrigger/#{token}"
param =  {
  WalletId: "0b2e0a83-9e47-4bd4-ac4b-c9b7b977aaa7",
  EventTriggered: "CustomerRegistered",
  PointRuleInput: {
    ProductQuantity: 1,
  },
}

ENV['API_KEY'] = nil
result = make_request(:post, apiUrl, param)
puts(result)
