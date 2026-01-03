#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']
email = "pjame.fb@gmail.com"

apiUrl = "api/Auth/org/#{orgId}/action/SendCustomerForgotPasswordEmail/#{email}"

result = make_request(:post, apiUrl, nil)
puts(result)
