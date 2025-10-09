#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'

$stdout.sync = true

# load_env.rb
File.readlines(".env", chomp: true).each do |line|
  next if line.strip.empty? || line.start_with?('#')

  key, value = line.split('=', 2)
  ENV[key] = value
  
end

def json?(str)
  JSON.parse(str)
  true
rescue JSON::ParserError
  false
end

def make_request(method, apiName, data)
  host = ENV['API_HTTP_ENDPOINT']
  apiKey = ENV['API_KEY']

  uri = URI.parse("#{host}/#{apiName}")  

  # แปลง method เช่น "post" → "Net::HTTP::Post"
  klass_name = "Net::HTTP::#{method.to_s.capitalize}"
  request_class = Object.const_get(klass_name)

  request = request_class.new(uri.request_uri)
  request['Content-Type'] = 'application/json'
  request.basic_auth("api", apiKey)

  if (!data.nil?)
    request.set_form_data(data)
  end

  http = Net::HTTP.new(uri.host, uri.port)  
  http.use_ssl = true

  response = http.request(request)

  if (response.code != '200')
    puts("ERROR : Failed to send request with error [#{response}]")
    return
  end

  result = response.body
  if json?(result)
    result = JSON.parse(result)
  end

  return result
end

def upload_file_to_gcs(presigned_url, file_path, content_type)
  uri = URI.parse(presigned_url)

  http = Net::HTTP.new(uri.host, uri.port)
  http.use_ssl = (uri.scheme == "https")

  request = Net::HTTP::Put.new(uri.request_uri)
  request['Content-Type'] = content_type
  request.body = File.read(file_path, mode: "rb")   # อ่านเป็น binary

  response = http.request(request)

  if response.is_a?(Net::HTTPSuccess)
    puts "✅ Upload สำเร็จ: #{file_path}"
  else
    puts "❌ Upload ล้มเหลว: #{response.code} #{response.message}"
    puts response.body
  end
end

################### Main #######################

orgId = "napbiotec"
itemId = "99b5dbd5-7a79-4560-9a89-8c24b0393229"
imageFile = "file_example_PNG_500kB.png"

apiGetPresignUrl = "api/Item/org/#{orgId}/action/GetItemImageUploadPresignedUrl/#{itemId}"
result = make_request(:get, apiGetPresignUrl, nil)

imagePath = result["imagePath"]
presignedUrl = result["presignedUrl"]

puts("Image path  : [#{imagePath}]")
puts("Presign URL : [#{presignedUrl}]")

upload_file_to_gcs(presignedUrl, imageFile, 'application/octet-stream')
