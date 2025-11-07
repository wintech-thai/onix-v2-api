#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']
id = '349788f4-64ba-4d65-9a97-0417a70294d8'

param =  {
  Name: "พี่เจมส์ นะจ๊ะ",
}

### UpdateUserById
apiUrl = "api/Customer/org/#{orgId}/action/UpdateCustomerById/#{id}"
result = make_request(:post, apiUrl, param)

puts(result)
