#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']
#id = '99207826-3b58-4f52-aecb-4254d6dc5b56'

apiUrl = "api/PointRule/org/#{orgId}/action/EvaluatePointRules/CustomerRegistered"
param = {
  ProductQuantity: 1,
}

result = make_request(:post, apiUrl, param)
puts(result)
